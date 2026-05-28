import {
  ArrowUp,
  ExternalLink,
  MessageCircle,
} from 'lucide-react'

import SentimentBadge from './SentimentBadge'

function RedditPostCard({ post }) {
  return (
    <article className="
      rounded-lg
      border
      border-zinc-200
      bg-zinc-50
      p-5
      dark:border-zinc-800
      dark:bg-zinc-950
    ">
      <div className="
        mb-4
        flex
        items-start
        justify-between
        gap-4
      ">
        <h3 className="
          text-lg
          font-bold
          leading-snug
          text-zinc-950
          dark:text-white
        ">
          {post.title}
        </h3>

        <SentimentBadge
          sentiment={post.sentiment}
        />
      </div>

      <div className="
        mb-4
        flex
        flex-wrap
        gap-3
        text-sm
        text-zinc-500
        dark:text-zinc-400
      ">
        <span>r/{post.subreddit}</span>

        <span>u/{post.author}</span>
      </div>

      <div className="
        mb-4
        flex
        gap-4
        text-sm
        font-semibold
        text-zinc-700
        dark:text-zinc-200
      ">
        <span className="flex items-center gap-1">
          <ArrowUp size={16} />
          {post.score}
        </span>

        <span className="flex items-center gap-1">
          <MessageCircle size={16} />
          {post.comments}
        </span>
      </div>

      <a
        href={post.url}
        target="_blank"
        rel="noreferrer"
        className="
          inline-flex
          items-center
          gap-2
          text-sm
          font-bold
          text-orange-600
          hover:text-orange-700
        "
      >
        Ver post
        <ExternalLink size={15} />
      </a>
    </article>
  )
}

export default RedditPostCard
